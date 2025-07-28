import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialogModule } from '@angular/material/dialog';
import { MatTableModule } from '@angular/material/table';
import { MatBadgeModule } from '@angular/material/badge';
import { TourService, Tour } from '../../../services/tour.service';
import { AuthService } from '../../../services/auth.service';
import { MapComponent } from '../../shared/map/map.component';

interface TourRoute {
  tourId: string;
  tourName: string;
  points: any[];
}

interface GuideReport {
  tourSales: TourSalesInfo[];
  bestRatedTour?: TourRatingInfo;
  worstRatedTour?: TourRatingInfo;
}

interface TourSalesInfo {
  tourId: string;
  name: string;
  salesCount: number;
}

interface TourRatingInfo {
  tourId: string;
  name: string;
  averageRating: number;
  ratingsCount: number;
}

@Component({
  selector: 'app-guide-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTabsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatTooltipModule,
    MatDialogModule,
    MatTableModule,
    MatBadgeModule,
    MapComponent
  ],
  templateUrl: './guide-dashboard.component.html',
  styleUrls: ['./guide-dashboard.component.scss']
})
export class GuideDashboardComponent implements OnInit {
  tours: Tour[] = [];
  isLoading = false;
  isCreatingTour = false;
  selectedState: string | null = null;
  showCreateForm = false;
  showKeyPointModal = false;
  showMap = false;
  showTourDetailsModal = false;
  tourRoutes: TourRoute[] = [];
  selectedTourForKeyPoint: Tour | null = null;
  selectedTourForDetails: Tour | null = null;

  // Report properties
  report: GuideReport | null = null;
  isReportLoading = false;
  selectedYear: number = new Date().getFullYear();
  selectedMonth: number = new Date().getMonth() + 1;
  salesColumns: string[] = ['name', 'salesCount'];

  newTour = {
    Name: '',
    Description: '',
    Difficulty: '',
    Category: '',
    Price: 0,
    Date: ''
  };

  newKeyPoint = {
    Name: '',
    Description: '',
    Latitude: 0,
    Longitude: 0,
    ImageUrl: ''
  };

  categories = [
    { value: 'Nature', label: 'Priroda' },
    { value: 'Art', label: 'Umetnost' },
    { value: 'Sport', label: 'Sport' },
    { value: 'Shopping', label: 'Kupovina' },
    { value: 'Food', label: 'Hrana' }
  ];

  difficulties = [
    { value: 'Easy', label: 'Lako' },
    { value: 'Medium', label: 'Srednje' },
    { value: 'Hard', label: 'Teško' }
  ];

  states = [
    { value: 'Draft', label: 'Skica' },
    { value: 'Published', label: 'Objavljeno' },
    { value: 'Cancelled', label: 'Otkazano' }
  ];

  months = [
    { value: 1, label: 'Januar' },
    { value: 2, label: 'Februar' },
    { value: 3, label: 'Mart' },
    { value: 4, label: 'April' },
    { value: 5, label: 'Maj' },
    { value: 6, label: 'Jun' },
    { value: 7, label: 'Jul' },
    { value: 8, label: 'Avgust' },
    { value: 9, label: 'Septembar' },
    { value: 10, label: 'Oktobar' },
    { value: 11, label: 'Novembar' },
    { value: 12, label: 'Decembar' }
  ];

  get availableYears(): number[] {
    const currentYear = new Date().getFullYear();
    const years = [];
    for (let i = currentYear; i >= currentYear - 5; i--) {
      years.push(i);
    }
    return years;
  }

  constructor(
    private tourService: TourService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadTours();
  }

  loadTours() {
    this.isLoading = true;
    this.tourService.getMyTours(this.selectedState).subscribe({
      next: (tours: Tour[]) => {
        setTimeout(() => {
          this.tours = tours;
          // Automatically load tour routes for map
          this.loadTourRoutes();
          this.isLoading = false;
        });
      },
      error: (error: any) => {
        console.error('Error loading tours:', error);
        setTimeout(() => {
          this.isLoading = false;
        });
      }
    });
  }

  loadReport() {
    if (!this.selectedYear || !this.selectedMonth) {
      return;
    }

    this.isReportLoading = true;
    this.report = null;

    this.tourService.getGuideReport(this.selectedYear, this.selectedMonth).subscribe({
      next: (report: GuideReport) => {
        this.report = report;
        this.isReportLoading = false;
        this.cdr.detectChanges();
      },
      error: (error: any) => {
        console.error('Error loading report:', error);
        this.isReportLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  getTotalSales(): number {
    if (!this.report?.tourSales) return 0;
    return this.report.tourSales.reduce((total, tour) => total + tour.salesCount, 0);
  }

  getStars(rating: number): number[] {
    const stars = [];
    const fullStars = Math.floor(rating);
    for (let i = 0; i < fullStars; i++) {
      stars.push(1);
    }
    return stars;
  }

  createTour() {
    if (this.newTour.Name && this.newTour.Description && this.newTour.Category && this.newTour.Difficulty && this.newTour.Price > 0 && this.newTour.Date) {
      this.isCreatingTour = true;
      
      // Dodaj GuideId iz ulogovanog korisnika
      const currentUser = this.authService.getUser();
      const tourData = {
        ...this.newTour,
        GuideId: currentUser?.id
      };
      
      console.log('Creating tour with data:', tourData);
      console.log('Current user:', currentUser);
      
      this.tourService.createTour(tourData).subscribe({
        next: () => {
          console.log('Tour created successfully');
          setTimeout(() => {
            this.onTourCreated();
            this.isCreatingTour = false;
          });
        },
        error: (error: any) => {
          console.error('Error creating tour:', error);
          this.isCreatingTour = false;
        }
      });
    }
  }

  onTourCreated() {
    this.showCreateForm = false;
    this.resetNewTour();
    this.loadTours();
  }

  resetNewTour() {
    this.newTour = {
      Name: '',
      Description: '',
      Difficulty: '',
      Category: '',
      Price: 0,
      Date: ''
    };
  }

  publishTour(tourId: string) {
    this.tourService.publishTour(tourId).subscribe({
      next: () => {
        console.log('Tour published successfully');
        this.loadTours();
      },
      error: (error: any) => {
        console.error('Error publishing tour:', error);
      }
    });
  }

  cancelTour(tourId: string) {
    this.tourService.cancelTour(tourId).subscribe({
      next: () => {
        console.log('Tour cancelled successfully');
        this.loadTours();
      },
      error: (error: any) => {
        console.error('Error cancelling tour:', error);
      }
    });
  }

  selectTourForKeyPoint(tour: Tour) {
    this.selectedTourForKeyPoint = tour;
    this.showKeyPointModal = true;
  }

  addKeyPoint() {
    if (this.selectedTourForKeyPoint && this.newKeyPoint.Name && this.newKeyPoint.Description) {
      const keyPointData = {
        tourId: this.selectedTourForKeyPoint.Id,
        name: this.newKeyPoint.Name,
        description: this.newKeyPoint.Description,
        latitude: this.newKeyPoint.Latitude,
        longitude: this.newKeyPoint.Longitude,
        imageUrl: this.newKeyPoint.ImageUrl
      };

      this.tourService.addKeyPoint(keyPointData).subscribe({
        next: () => {
          console.log('Key point added successfully');
          this.showKeyPointModal = false;
          this.resetNewKeyPoint();
          this.loadTours();
        },
        error: (error: any) => {
          console.error('Error adding key point:', error);
        }
      });
    }
  }

  createKeyPoint() {
    this.addKeyPoint();
  }

  closeKeyPointModal() {
    this.showKeyPointModal = false;
    this.resetNewKeyPoint();
  }

  resetNewKeyPoint() {
    this.newKeyPoint = {
      Name: '',
      Description: '',
      Latitude: 0,
      Longitude: 0,
      ImageUrl: ''
    };
    this.selectedTourForKeyPoint = null;
  }

  showTourDetails(tour: Tour) {
    this.selectedTourForDetails = tour;
    this.showTourDetailsModal = true;
  }

  closeTourDetailsModal() {
    this.showTourDetailsModal = false;
    this.selectedTourForDetails = null;
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

  getStateLabel(state: string): string {
    switch (state) {
      case 'Draft': return 'Skica';
      case 'Published': return 'Objavljeno';
      case 'Cancelled': return 'Otkazano';
      default: return state;
    }
  }

  getCategoryLabel(category: string): string {
    switch (category) {
      case 'Nature': return 'Priroda';
      case 'Art': return 'Umetnost';
      case 'Sport': return 'Sport';
      case 'Shopping': return 'Kupovina';
      case 'Food': return 'Hrana';
      default: return category;
    }
  }

  getDifficultyLabel(difficulty: string): string {
    switch (difficulty) {
      case 'Easy': return 'Lako';
      case 'Medium': return 'Srednje';
      case 'Hard': return 'Teško';
      default: return difficulty;
    }
  }
} 