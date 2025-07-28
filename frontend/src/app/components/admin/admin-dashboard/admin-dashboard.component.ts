import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialogModule } from '@angular/material/dialog';
import { AuthService, User } from '../../../services/auth.service';
import { AdminService } from '../../../services/admin.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTabsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatTooltipModule,
    MatChipsModule,
    MatDialogModule
  ],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss']
})
export class AdminDashboardComponent implements OnInit {
  maliciousUsers: User[] = [];
  allProblems: any[] = [];
  isLoading = false;
  problemsLoading = false;

  displayedColumns: string[] = ['username', 'email', 'firstName', 'lastName', 'role', 'isMalicious', 'isBlocked', 'actions'];
  problemColumns: string[] = ['tourName', 'touristName', 'description', 'status', 'actions'];

  constructor(
    private adminService: AdminService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.loadMaliciousUsers();
    this.loadAllProblems();
  }

  loadMaliciousUsers() {
    this.isLoading = true;
    this.adminService.getMaliciousUsers().subscribe({
      next: (users: User[]) => {
        this.maliciousUsers = users;
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error loading malicious users:', error);
        this.isLoading = false;
      }
    });
  }

  loadAllProblems() {
    this.problemsLoading = true;
    this.adminService.getAllProblems().subscribe({
      next: (problems: any[]) => {
        this.allProblems = problems;
        this.problemsLoading = false;
      },
      error: (error: any) => {
        console.error('Error loading problems:', error);
        this.problemsLoading = false;
      }
    });
  }

  blockUser(userId: string) {
    this.adminService.blockUser(userId).subscribe({
      next: () => {
        this.loadMaliciousUsers();
      },
      error: (error: any) => {
        console.error('Error blocking user:', error);
      }
    });
  }

  unblockUser(userId: string) {
    this.adminService.unblockUser(userId).subscribe({
      next: () => {
        this.loadMaliciousUsers();
      },
      error: (error: any) => {
        console.error('Error unblocking user:', error);
      }
    });
  }

  updateProblemStatus(problemId: string, status: string) {
    this.adminService.updateProblemStatus(problemId, status).subscribe({
      next: () => {
        this.loadAllProblems();
      },
      error: (error: any) => {
        console.error('Error updating problem status:', error);
      }
    });
  }

  getStatusLabel(status: string): string {
    switch (status) {
      case 'Reported': return 'Prijavljeno';
      case 'InProgress': return 'U toku';
      case 'Resolved': return 'Rešeno';
      case 'Closed': return 'Zatvoreno';
      default: return status;
    }
  }

  getStatusColor(status: string): string {
    switch (status) {
      case 'Reported': return 'warn';
      case 'InProgress': return 'accent';
      case 'Resolved': return 'primary';
      case 'Closed': return '';
      default: return '';
    }
  }


} 