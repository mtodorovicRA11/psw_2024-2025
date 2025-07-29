import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TourService } from '../../../services/tour.service';

@Component({
  selector: 'app-report-problem',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './report-problem.component.html',
  styleUrls: ['./report-problem.component.scss']
})
export class ReportProblemComponent {
  @Input() tourId: string = '';
  @Input() tourName: string = '';
  @Output() problemReported = new EventEmitter<void>();

  problem = {
    title: '',
    description: ''
  };

  isLoading = false;
  errorMessage = '';

  constructor(private tourService: TourService) {}

  submitProblem() {
    if (this.problem.title.trim() && this.problem.description.trim()) {
      this.isLoading = true;
      this.errorMessage = '';

      const problemRequest = {
        tourId: this.tourId,
        title: this.problem.title,
        description: this.problem.description
      };

      this.tourService.reportProblem(problemRequest).subscribe({
        next: () => {
          this.isLoading = false;
          this.problemReported.emit();
        },
        error: (error: any) => {
          this.isLoading = false;
          this.errorMessage = error.error?.error || 'Greška pri prijavljivanju problema';
          console.error('Error reporting problem:', error);
        }
      });
    }
  }
} 