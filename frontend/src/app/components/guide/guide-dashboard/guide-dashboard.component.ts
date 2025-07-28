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
import { TourService, Tour } from '../../../services/tour.service';
import { AuthService } from '../../../services/auth.service';
import { MapComponent, TourRoute, MapPoint } from '../../shared/map/map.component';

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
  tourRoutes: TourRoute[] = [];
  selectedTourForKeyPoint: Tour | null = null;

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

  publishTour(tourId: string) {
    this.tourService.publishTour(tourId).subscribe({
      next: () => {
        console.log('Tour published successfully');
        setTimeout(() => {
          this.loadTours();
        });
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
        setTimeout(() => {
          this.loadTours();
        });
      },
      error: (error: any) => {
        console.error('Error cancelling tour:', error);
      }
    });
  }

  onStateChange() {
    this.loadTours();
  }

  resetForm() {
    this.newTour = {
      Name: '',
      Description: '',
      Difficulty: '',
      Category: '',
      Price: 0,
      Date: ''
    };
  }

  onTourCreated() {
    this.resetForm();
    setTimeout(() => {
      this.loadTours();
      this.showCreateForm = false;
    });
  }

  addKeyPoint(tour: Tour) {
    this.selectedTourForKeyPoint = tour;
    this.showKeyPointModal = true;
    this.resetKeyPointForm();
  }

  closeKeyPointModal() {
    this.showKeyPointModal = false;
    this.selectedTourForKeyPoint = null;
    this.resetKeyPointForm();
  }

  createKeyPoint() {
    if (this.selectedTourForKeyPoint && this.newKeyPoint.Name && this.newKeyPoint.Description) {
      const keyPointData = {
        TourId: this.selectedTourForKeyPoint.Id,
        Name: this.newKeyPoint.Name,
        Description: this.newKeyPoint.Description,
        Latitude: this.newKeyPoint.Latitude,
        Longitude: this.newKeyPoint.Longitude,
        ImageUrl: this.newKeyPoint.ImageUrl || null
      };

      this.tourService.addKeyPoint(keyPointData).subscribe({
        next: () => {
          console.log('Key point added successfully');
          this.closeKeyPointModal();
          setTimeout(() => {
            this.loadTours(); // Reload tours to get updated key points
          });
        },
        error: (error: any) => {
          console.error('Error adding key point:', error);
        }
      });
    }
  }

  resetKeyPointForm() {
    this.newKeyPoint = {
      Name: '',
      Description: '',
      Latitude: 0,
      Longitude: 0,
      ImageUrl: ''
    };
  }



  getStateLabel(state: string): string {
    const stateObj = this.states.find(s => s.value === state);
    return stateObj ? stateObj.label : state;
  }

  getCategoryLabel(category: string): string {
    const categoryObj = this.categories.find(c => c.value === category);
    return categoryObj ? categoryObj.label : category;
  }

  getDifficultyLabel(difficulty: string): string {
    const difficultyObj = this.difficulties.find(d => d.value === difficulty);
    return difficultyObj ? difficultyObj.label : difficulty;
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
} 