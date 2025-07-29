import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTableModule } from '@angular/material/table';
import { TourService, Tour, Cart } from '../../../services/tour.service';
import { AuthService } from '../../../services/auth.service';
import { RateTourComponent } from '../rate-tour/rate-tour.component';
import { ReportProblemComponent } from '../report-problem/report-problem.component';
import { MapComponent, TourRoute, MapPoint } from '../../shared/map/map.component';

@Component({
  selector: 'app-tourist-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    MatTabsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatBadgeModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatTooltipModule,
    MatTableModule,
    RateTourComponent,
    ReportProblemComponent,
    MapComponent
  ],
  templateUrl: './tourist-dashboard.component.html',
  styleUrls: ['./tourist-dashboard.component.scss']
})
export class TouristDashboardComponent implements OnInit {
  tours: Tour[] = [];
  cart: Cart = { items: [], totalPrice: 0, maxUsableBonusPoints: 0 };
  isLoading = false;
  isCartLoading = false;
  selectedCategory: string | null = null;
  onlyAwardedGuides = false;
  showRateModal = false;
  showProblemModal = false;
  showTourDetailsModal = false;
  selectedTour: Tour | null = null;
  showMap = false;
  tourRoutes: TourRoute[] = [];
  
  // Problem-related properties
  myProblems: any[] = [];
  problemsLoading = false;
  problemColumns = ['tourName', 'title', 'description', 'status', 'createdAt'];
  
  // Bonus points
  useBonusPoints: number = 0;

  // Purchased tours
  purchasedTours: Tour[] = [];
  purchasedToursLoading = false;
  purchasedToursColumns = ['tourName', 'date', 'price', 'status', 'actions'];

  // Profile
  userInterests: string[] = [];
  isUpdatingInterests = false;
  availableInterests = [
    { value: 'Nature', label: 'Priroda' },
    { value: 'Art', label: 'Umetnost' },
    { value: 'Sport', label: 'Sport' },
    { value: 'Shopping', label: 'Kupovina' },
    { value: 'Food', label: 'Hrana' }
  ];

  categories = [
    { value: 'Nature', label: 'Priroda' },
    { value: 'Art', label: 'Umetnost' },
    { value: 'Sport', label: 'Sport' },
    { value: 'Shopping', label: 'Kupovina' },
    { value: 'Food', label: 'Hrana' }
  ];

  constructor(
    private tourService: TourService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    // Proveri da li je korisnik ulogovan
    if (!this.authService.isLoggedIn()) {
      console.log('User not logged in, redirecting to login');
      this.authService.logout();
      return;
    }
    
    // Debug: proveri da li je token sačuvan
    const token = this.authService.getToken();
    console.log('Token in tourist dashboard:', token);
    
    // Osiguraj da je cart inicijalizovan odmah
    this.cart = { items: [], totalPrice: 0, maxUsableBonusPoints: 0 };
    
    // Postavi loading stanje odmah
    this.isLoading = true;
    this.isCartLoading = true;
    this.cdr.detectChanges();
    
    this.loadTours();
    this.loadCart();
    this.loadMyProblems();
    this.loadPurchasedTours();
  }

  loadTours() {
    this.tourService.getPublishedTours(this.selectedCategory, null, this.onlyAwardedGuides).subscribe({
      next: (tours: Tour[]) => {
        console.log('Loaded tours:', tours);
        console.log('First tour ID:', tours[0]?.Id);
        console.log('First tour ID type:', typeof tours[0]?.Id);
        this.tours = tours;
        // Automatically load tour routes for map
        this.loadTourRoutes();
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (error: any) => {
        console.error('Error loading tours:', error);
        
        // Ako je 401 Unauthorized, redirektuj na login
        if (error.status === 401) {
          console.log('Unauthorized access, redirecting to login');
          this.authService.logout();
          return;
        }
        
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadCart() {
    this.tourService.getCart().subscribe({
      next: (cart: any) => {
        console.log('Cart data received:', cart);
        console.log('Cart items:', cart?.Items);
        console.log('Cart items length:', cart?.Items?.length);
        
        // Mapiraj PascalCase svojstva iz backend-a na camelCase svojstva
        const mappedItems = (cart?.Items || []).map((item: any) => ({
          tourId: item.TourId,
          name: item.Name,
          price: item.Price,
          guideName: item.GuideName,
          date: item.Date,
          category: item.Category
        }));
        
        this.cart = {
          items: mappedItems,
          totalPrice: cart?.TotalPrice || 0,
          maxUsableBonusPoints: cart?.MaxUsableBonusPoints || 0
        };
        console.log('Updated cart in component:', this.cart);
        console.log('Updated cart items length:', this.cart.items.length);
        
        // Reset bonus points when cart is loaded
        this.useBonusPoints = 0;
        
        this.isCartLoading = false;
        this.cdr.detectChanges();
      },
      error: (error: any) => {
        console.error('Error loading cart:', error);
        
        // Ako je 401 Unauthorized, redirektuj na login
        if (error.status === 401) {
          console.log('Unauthorized access, redirecting to login');
          this.authService.logout();
          return;
        }
        
        // Postavi prazan cart ako dođe do greške
        this.cart = {
          items: [],
          totalPrice: 0,
          maxUsableBonusPoints: 0
        };
        this.isCartLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  addToCart(tourId: string) {
    console.log('Adding to cart, tourId:', tourId);
    console.log('TourId type:', typeof tourId);
    
    if (!tourId) {
      console.error('TourId is null or undefined');
      return;
    }
    
    // Postavi loading stanje samo ako nije već postavljeno
    if (!this.isCartLoading) {
      this.isCartLoading = true;
    }
    
    this.tourService.addToCart(tourId).subscribe({
      next: () => {
        console.log('Successfully added tour to cart, tourId:', tourId);
        console.log('Calling loadCart after adding to cart');
        this.loadCart();
      },
      error: (error: any) => {
        console.error('Error adding to cart:', error);
        
        // Ako je 401 Unauthorized, redirektuj na login
        if (error.status === 401) {
          console.log('Unauthorized access, redirecting to login');
          this.authService.logout();
          return;
        }
        
        this.isCartLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  removeFromCart(tourId: string) {
    // Postavi loading stanje samo ako nije već postavljeno
    if (!this.isCartLoading) {
      this.isCartLoading = true;
    }
    
    this.tourService.removeFromCart(tourId).subscribe({
      next: () => {
        console.log('Successfully removed tour from cart, tourId:', tourId);
        console.log('Calling loadCart after removing from cart');
        this.loadCart();
      },
      error: (error: any) => {
        console.error('Error removing from cart:', error);
        
        // Ako je 401 Unauthorized, redirektuj na login
        if (error.status === 401) {
          console.log('Unauthorized access, redirecting to login');
          this.authService.logout();
          return;
        }
        
        this.isCartLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  purchaseCart() {
    if (this.cart?.items && this.cart.items.length > 0) {
      this.isCartLoading = true;
      
      // Calculate bonus points per tour (distribute evenly)
      const bonusPointsPerTour = Math.floor(this.useBonusPoints / this.cart.items.length);
      const remainingBonusPoints = this.useBonusPoints % this.cart.items.length;
      
      let completedPurchases = 0;
      let hasError = false;
      
      // Purchase each tour individually with bonus points
      this.cart.items.forEach((item: any, index: number) => {
        const bonusPointsForThisTour = bonusPointsPerTour + (index < remainingBonusPoints ? 1 : 0);
        
        this.tourService.purchaseSingleTour(item.tourId, bonusPointsForThisTour).subscribe({
          next: () => {
            completedPurchases++;
            if (completedPurchases === this.cart.items.length && !hasError) {
              // All purchases completed successfully
              this.useBonusPoints = 0; // Reset bonus points
              this.loadCart();
              this.loadTours();
              this.isCartLoading = false;
              this.cdr.detectChanges();
            }
          },
          error: (error: any) => {
            console.error('Error purchasing tour:', error);
            hasError = true;
            
            // Ako je 401 Unauthorized, redirektuj na login
            if (error.status === 401) {
              console.log('Unauthorized access, redirecting to login');
              this.authService.logout();
              return;
            }
            
            this.isCartLoading = false;
            this.cdr.detectChanges();
          }
        });
      });
    }
  }

  onCategoryChange() {
    this.loadTours();
  }

  onAwardedGuidesChange() {
    this.loadTours();
  }



  rateTour(tour: Tour) {
    this.selectedTour = tour;
    this.showRateModal = true;
  }

  reportProblem(tour: Tour) {
    this.selectedTour = tour;
    this.showProblemModal = true;
  }

  closeRateModal() {
    this.showRateModal = false;
    this.selectedTour = null;
  }

  closeProblemModal() {
    this.showProblemModal = false;
    this.selectedTour = null;
  }

  closeTourDetailsModal() {
    this.showTourDetailsModal = false;
    this.selectedTour = null;
  }

  onRatingSubmitted() {
    this.closeRateModal();
  }

  onProblemReported() {
    this.closeProblemModal();
    this.loadMyProblems(); // Reload problems after reporting
    this.cdr.detectChanges();
  }

  onBonusPointsChange() {
    // Ensure the value is within valid range
    if (this.useBonusPoints < 0) {
      this.useBonusPoints = 0;
    }
    if (this.cart && this.useBonusPoints > this.cart.maxUsableBonusPoints) {
      this.useBonusPoints = this.cart.maxUsableBonusPoints;
    }
    this.cdr.detectChanges();
  }

  getFinalPrice(): number {
    if (!this.cart) return 0;
    const finalPrice = this.cart.totalPrice - this.useBonusPoints;
    return Math.max(0, finalPrice);
  }

  loadMyProblems() {
    this.problemsLoading = true;
    this.tourService.getMyProblems().subscribe({
      next: (problems: any[]) => {
        console.log('My problems loaded:', problems);
        this.myProblems = problems;
        this.problemsLoading = false;
        this.cdr.detectChanges();
      },
      error: (error: any) => {
        console.error('Error loading my problems:', error);
        this.problemsLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  getProblemStatusColor(status: string): string {
    switch (status) {
      case 'Pending': return 'warn';
      case 'Resolved': return 'primary';
      case 'UnderReview': return 'accent';
      case 'Rejected': return 'warn';
      default: return 'primary';
    }
  }

  getProblemStatusLabel(status: string): string {
    switch (status) {
      case 'Pending': return 'Na čekanju';
      case 'Resolved': return 'Rešeno';
      case 'UnderReview': return 'Na reviziji';
      case 'Rejected': return 'Odbacen';
      default: return status;
    }
  }

  toggleMap() {
    this.showMap = !this.showMap;
    if (this.showMap) {
      this.loadTourRoutes();
    }
  }

  loadTourRoutes() {
    this.tourRoutes = this.tours.map(tour => ({
      tourId: tour.Id,
      tourName: tour.Name,
      points: tour.KeyPoints?.map((point: any) => ({
        id: point.Id,
        name: point.Name,
        description: point.Description,
        latitude: point.Latitude,
        longitude: point.Longitude
      })) || []
    })).filter(route => route.points.length > 0);
  }

  selectTourForDetails(tourId: string) {
    // Find the tour in the loaded tours
    const tour = this.tours.find(t => t.Id === tourId);
    if (tour) {
      this.selectedTour = tour;
      this.showTourDetailsModal = true;
    } else {
      // If tour is not in loaded tours, try to fetch it
      this.tourService.getPublishedTours().subscribe(tours => {
        const foundTour = tours.find(t => t.Id === tourId);
        if (foundTour) {
          this.selectedTour = foundTour;
          this.showTourDetailsModal = true;
        }
      });
    }
  }

  loadPurchasedTours() {
    this.purchasedToursLoading = true;
    this.tourService.getPurchasedTours().subscribe({
      next: (tours: Tour[]) => {
        this.purchasedTours = tours;
        this.purchasedToursLoading = false;
        this.cdr.detectChanges();
      },
      error: (error: any) => {
        console.error('Error loading purchased tours:', error);
        this.purchasedToursLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  canRateTour(tour: Tour): boolean {
    const tourDate = new Date(tour.Date);
    const now = new Date();
    const daysSinceTour = (now.getTime() - tourDate.getTime()) / (1000 * 60 * 60 * 24);
    
    // Može oceniti samo ako je tura završena i nije prošlo više od 30 dana
    return tourDate < now && daysSinceTour <= 30;
  }

  getTourStatusColor(tour: Tour): string {
    const tourDate = new Date(tour.Date);
    const now = new Date();
    
    if (tourDate > now) {
      return 'primary'; // Budućnost
    } else if (this.canRateTour(tour)) {
      return 'accent'; // Može oceniti
    } else {
      return 'warn'; // Ne može oceniti
    }
  }

  getTourStatusLabel(tour: Tour): string {
    const tourDate = new Date(tour.Date);
    const now = new Date();
    
    if (tourDate > now) {
      return 'Predstoji';
    } else if (this.canRateTour(tour)) {
      return 'Može oceniti';
    } else {
      return 'Ne može oceniti';
    }
  }

  onInterestChange(interest: string, checked: boolean) {
    if (checked && !this.userInterests.includes(interest)) {
      this.userInterests.push(interest);
    } else if (!checked && this.userInterests.includes(interest)) {
      this.userInterests = this.userInterests.filter(i => i !== interest);
    }
  }

  updateInterests() {
    this.isUpdatingInterests = true;
    this.authService.updateInterests(this.userInterests).subscribe({
      next: () => {
        this.isUpdatingInterests = false;
        this.cdr.detectChanges();
      },
      error: (error: any) => {
        console.error('Error updating interests:', error);
        this.isUpdatingInterests = false;
        this.cdr.detectChanges();
      }
    });
  }
} 