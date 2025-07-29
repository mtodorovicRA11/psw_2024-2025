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
import { MatChipsModule } from '@angular/material/chips';
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
    MatChipsModule,
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
  
  // Problem-related properties
  myProblems: any[] = [];
  problemsLoading = false;
  problemColumns = ['tourName', 'touristName', 'title', 'description', 'status', 'actions'];
  selectedProblemForEvents: any = null;
  problemEvents: any[] = [];
  eventsLoading = false;
  showEventsModal = false;

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
    this.loadMyProblems();
  }

  loadTours() {
    this.isLoading = true;
    this.tourService.getMyTours(this.selectedState).subscribe({
      next: (tours: Tour[]) => {
        this.tours = tours;
        // Automatically load tour routes for map
        this.loadTourRoutes();
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (error: any) => {
        console.error('Error loading tours:', error);
        this.isLoading = false;
        this.cdr.detectChanges();
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
          this.onTourCreated();
          this.isCreatingTour = false;
          this.cdr.detectChanges();
        },
        error: (error: any) => {
          console.error('Error creating tour:', error);
          this.isCreatingTour = false;
          this.cdr.detectChanges();
        }
      });
    }
  }

  onTourCreated() {
    this.showCreateForm = false;
    this.resetNewTour();
    this.loadTours();
    this.cdr.detectChanges();
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
    // Find the tour and update its loading state
    const tour = this.tours.find(t => t.Id === tourId);
    if (tour) {
      tour.isPublishing = true;
      this.cdr.detectChanges();
    }

    this.tourService.publishTour(tourId).subscribe({
      next: () => {
        console.log('Tour published successfully');
        if (tour) {
          tour.isPublishing = false;
        }
        this.loadTours();
        this.cdr.detectChanges();
      },
      error: (error: any) => {
        console.error('Error publishing tour:', error);
        if (tour) {
          tour.isPublishing = false;
        }
        this.cdr.detectChanges();
      }
    });
  }

  cancelTour(tourId: string) {
    // Find the tour and update its loading state
    const tour = this.tours.find(t => t.Id === tourId);
    if (tour) {
      tour.isCancelling = true;
      this.cdr.detectChanges();
    }

    this.tourService.cancelTour(tourId).subscribe({
      next: () => {
        console.log('Tour cancelled successfully');
        if (tour) {
          tour.isCancelling = false;
        }
        this.loadTours();
        this.cdr.detectChanges();
      },
      error: (error: any) => {
        console.error('Error cancelling tour:', error);
        if (tour) {
          tour.isCancelling = false;
        }
        this.cdr.detectChanges();
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
          this.cdr.detectChanges();
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

  loadMyProblems() {
    this.problemsLoading = true;
    this.tourService.getGuideProblems().subscribe({
      next: (problems: any[]) => {
        console.log('Guide problems loaded:', problems);
        this.myProblems = problems;
        this.problemsLoading = false;
        this.cdr.detectChanges();
      },
      error: (error: any) => {
        console.error('Error loading guide problems:', error);
        this.problemsLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  updateProblemStatus(problemId: string, newStatus: string) {
    this.tourService.updateProblemStatus(problemId, newStatus).subscribe({
      next: () => {
        console.log('Problem status updated successfully');
        this.loadMyProblems(); // Reload problems after update
        this.cdr.detectChanges();
      },
      error: (error: any) => {
        console.error('Error updating problem status:', error);
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

  getAvailableStatusOptions(currentStatus: string): string[] {
    switch (currentStatus) {
      case 'Pending':
        return ['Resolved', 'UnderReview'];
      case 'UnderReview':
        return ['Pending', 'Rejected'];
      default:
        return [];
    }
  }

  getStatusTransitionLabel(currentStatus: string, newStatus: string): string {
    switch (newStatus) {
      case 'Resolved':
        return 'Označi kao rešen';
      case 'UnderReview':
        return 'Pošalji na reviziju';
      case 'Pending':
        return 'Vrati na čekanje';
      case 'Rejected':
        return 'Odbaci problem';
      default:
        return newStatus;
    }
  }

  getStatusButtonColor(status: string): string {
    switch (status) {
      case 'Resolved':
        return 'primary';
      case 'UnderReview':
        return 'accent';
      case 'Pending':
        return 'primary';
      case 'Rejected':
        return 'warn';
      default:
        return 'primary';
    }
  }

  getStatusIcon(status: string): string {
    switch (status) {
      case 'Resolved':
        return 'check';
      case 'UnderReview':
        return 'send';
      case 'Pending':
        return 'schedule';
      case 'Rejected':
        return 'close';
      default:
        return 'help';
    }
  }

  showProblemEvents(problem: any) {
    this.selectedProblemForEvents = problem;
    this.showEventsModal = true;
    this.eventsLoading = true; // Inicijalizujemo loading stanje
    this.problemEvents = []; // Resetujemo events
    // Koristimo setTimeout da izbegnemo ExpressionChangedAfterItHasBeenCheckedError
    setTimeout(() => {
      this.loadProblemEvents(problem.Id);
    }, 0);
  }

  loadProblemEvents(problemId: string) {
    this.tourService.getProblemEvents(problemId).subscribe({
      next: (events: any[]) => {
        this.problemEvents = events;
        this.eventsLoading = false;
        this.cdr.detectChanges();
      },
      error: (error: any) => {
        console.error('Error loading problem events:', error);
        this.eventsLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  closeEventsModal() {
    this.showEventsModal = false;
    this.selectedProblemForEvents = null;
    this.problemEvents = [];
    this.eventsLoading = false; // Resetujemo loading stanje
  }

  getEventTypeLabel(eventType: string): string {
    switch (eventType) {
      case 'ProblemCreatedEvent':
        return 'Problem kreiran';
      case 'ProblemStatusChangedEvent':
        return 'Status promenjen';
      default:
        return eventType;
    }
  }

  getEventUserRoleLabel(userRole: string): string {
    switch (userRole) {
      case 'Tourist':
        return 'Turista';
      case 'Guide':
        return 'Vodič';
      case 'Admin':
        return 'Administrator';
      default:
        return userRole;
    }
  }

  formatEventDate(date: string): string {
    try {
      if (!date) return 'Nepoznat datum';
      const dateObj = new Date(date);
      if (isNaN(dateObj.getTime())) return 'Neispravan datum';
      return dateObj.toLocaleString('sr-RS');
    } catch (error) {
      console.error('Error formatting date:', error, date);
      return 'Greška pri formatiranju datuma';
    }
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Pending':
        return 'status-pending';
      case 'Resolved':
        return 'status-resolved';
      case 'UnderReview':
        return 'status-under-review';
      case 'Rejected':
        return 'status-rejected';
      default:
        return 'status-pending';
    }
  }
} 