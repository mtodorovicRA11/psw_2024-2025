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
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTooltipModule } from '@angular/material/tooltip';
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
    MatSelectModule,
    MatCheckboxModule,
    MatTooltipModule,
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
    
    this.loadTours();
    this.loadCart();
  }

  loadTours() {
    this.tourService.getPublishedTours(this.selectedCategory, null, this.onlyAwardedGuides).subscribe({
      next: (tours: Tour[]) => {
        console.log('Loaded tours:', tours);
        console.log('First tour ID:', tours[0]?.Id);
        console.log('First tour ID type:', typeof tours[0]?.Id);
        setTimeout(() => {
          this.tours = tours;
          // Automatically load tour routes for map
          this.loadTourRoutes();
          this.isLoading = false;
        });
      },
      error: (error: any) => {
        console.error('Error loading tours:', error);
        
        // Ako je 401 Unauthorized, redirektuj na login
        if (error.status === 401) {
          console.log('Unauthorized access, redirecting to login');
          this.authService.logout();
          return;
        }
        
        setTimeout(() => {
          this.isLoading = false;
        });
      }
    });
  }

  loadCart() {
    this.tourService.getCart().subscribe({
      next: (cart: any) => {
        console.log('Cart data received:', cart);
        console.log('Cart items:', cart?.Items);
        console.log('Cart items length:', cart?.Items?.length);
        setTimeout(() => {
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
          this.isCartLoading = false;
        });
      },
      error: (error: any) => {
        console.error('Error loading cart:', error);
        
        // Ako je 401 Unauthorized, redirektuj na login
        if (error.status === 401) {
          console.log('Unauthorized access, redirecting to login');
          this.authService.logout();
          return;
        }
        
        setTimeout(() => {
          // Postavi prazan cart ako dođe do greške
          this.cart = {
            items: [],
            totalPrice: 0,
            maxUsableBonusPoints: 0
          };
          this.isCartLoading = false;
        });
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
        setTimeout(() => {
          console.log('Calling loadCart after adding to cart');
          this.loadCart();
        }, 100); // Dodaj mali delay da se osigura da je backend ažurirao korpu
      },
      error: (error: any) => {
        console.error('Error adding to cart:', error);
        
        // Ako je 401 Unauthorized, redirektuj na login
        if (error.status === 401) {
          console.log('Unauthorized access, redirecting to login');
          this.authService.logout();
          return;
        }
        
        setTimeout(() => {
          this.isCartLoading = false;
        });
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
        setTimeout(() => {
          console.log('Calling loadCart after removing from cart');
          this.loadCart();
        }, 100); // Dodaj mali delay da se osigura da je backend ažurirao korpu
      },
      error: (error: any) => {
        console.error('Error removing from cart:', error);
        
        // Ako je 401 Unauthorized, redirektuj na login
        if (error.status === 401) {
          console.log('Unauthorized access, redirecting to login');
          this.authService.logout();
          return;
        }
        
        setTimeout(() => {
          this.isCartLoading = false;
        });
      }
    });
  }

  purchaseCart() {
    if (this.cart?.items && this.cart.items.length > 0) {
      // Postavi loading stanje samo ako nije već postavljeno
      if (!this.isCartLoading) {
        this.isCartLoading = true;
      }
      
      const purchaseRequest = {
        tourIds: this.cart.items.map((item: any) => item.tourId)
      };
      
      this.tourService.purchaseTours(purchaseRequest).subscribe({
        next: () => {
          setTimeout(() => {
            this.loadCart();
            // Reload tours to update availability
            this.loadTours();
          });
        },
        error: (error: any) => {
          console.error('Error purchasing tours:', error);
          
          // Ako je 401 Unauthorized, redirektuj na login
          if (error.status === 401) {
            console.log('Unauthorized access, redirecting to login');
            this.authService.logout();
            return;
          }
          
          setTimeout(() => {
            this.isCartLoading = false;
          });
        }
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
} 