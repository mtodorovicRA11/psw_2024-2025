import { Component, Input, Output, EventEmitter, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TourService } from '../../../services/tour.service';

@Component({
  selector: 'app-rate-tour',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './rate-tour.component.html',
  styleUrls: ['./rate-tour.component.scss']
})
export class RateTourComponent {
  @Input() tourId: string = '';
  @Input() tourName: string = '';
  @Output() ratingSubmitted = new EventEmitter<void>();

  rating = {
    rating: 5,
    comment: ''
  };

  isLoading = false;
  errorMessage = '';

  constructor(
    private tourService: TourService,
    private cdr: ChangeDetectorRef
  ) {}

  submitRating() {
    if (this.rating.rating && this.rating.comment.trim()) {
      this.isLoading = true;
      this.errorMessage = '';

      const ratingRequest = {
        tourId: this.tourId,
        rating: this.rating.rating,
        comment: this.rating.comment
      };

      this.tourService.rateTour(ratingRequest).subscribe({
        next: () => {
          this.isLoading = false;
          this.ratingSubmitted.emit();
        },
        error: (error: any) => {
          this.isLoading = false;
          console.error('Error rating tour:', error);
          console.log('Error response:', error.error);
          
          // Use setTimeout to avoid ExpressionChangedAfterItHasBeenCheckedError
          setTimeout(() => {
            this.errorMessage = error.error?.error || 'Greška pri ocenjivanju';
            console.log('Setting error message:', this.errorMessage);
            this.cdr.detectChanges();
          }, 0);
        }
      });
    }
  }

  onStarClick(star: number) {
    this.rating.rating = star;
  }
} 