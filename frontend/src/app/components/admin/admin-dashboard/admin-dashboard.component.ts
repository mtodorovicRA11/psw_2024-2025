import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
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

interface MaliciousUser {
  id: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  interests: string[];
  bonusPoints: number;
  isMalicious: boolean;
  isBlocked: boolean;
}

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
  maliciousUsers: MaliciousUser[] = [];
  allProblems: any[] = [];
  isLoading = false;
  problemsLoading = false;

  displayedColumns: string[] = ['username', 'email', 'firstName', 'lastName', 'role', 'isMalicious', 'isBlocked', 'actions'];
  problemColumns: string[] = ['tourName', 'touristName', 'description', 'status', 'actions'];

  constructor(
    private adminService: AdminService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    // Postavi loading stanje i pokreni učitavanje
    this.isLoading = true;
    this.problemsLoading = true;
    this.cdr.detectChanges();
    
    this.loadMaliciousUsers();
    this.loadAllProblems();
  }

  loadMaliciousUsers() {
    this.adminService.getMaliciousUsers().subscribe({
      next: (users: any[]) => {
        console.log('Raw malicious users from backend:', users);
        // Mapiraj PascalCase svojstva iz backend-a na camelCase svojstva
        this.maliciousUsers = users.map(user => {
          console.log('Mapping user:', user);
          const mappedUser = {
            id: user.Id,
            username: user.Username,
            email: user.Email,
            firstName: user.FirstName,
            lastName: user.LastName,
            role: user.Role,
            interests: user.Interests || [],
            bonusPoints: user.BonusPoints || 0,
            isMalicious: user.IsMalicious || false,
            isBlocked: user.IsBlocked || false
          };
          console.log('Mapped user:', mappedUser);
          return mappedUser;
        });
        console.log('Mapped malicious users:', this.maliciousUsers);
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (error: any) => {
        console.error('Error loading malicious users:', error);
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadAllProblems() {
    this.adminService.getAllProblems().subscribe({
      next: (problems: any[]) => {
        console.log('Raw problems from backend:', problems);
        this.allProblems = problems;
        console.log('Problems loaded:', this.allProblems);
        console.log('Problem statuses:', this.allProblems.map(p => ({ id: p.Id, status: p.Status })));
        this.problemsLoading = false;
        this.cdr.detectChanges();
      },
      error: (error: any) => {
        console.error('Error loading problems:', error);
        this.problemsLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  blockUser(userId: string) {
    console.log('Blocking user with ID:', userId);
    if (!userId || userId === 'undefined') {
      console.error('Invalid user ID:', userId);
      return;
    }
    
    this.adminService.blockUser(userId).subscribe({
      next: () => {
        console.log('User blocked successfully');
        this.loadMaliciousUsers();
      },
      error: (error: any) => {
        console.error('Error blocking user:', error);
      }
    });
  }

  unblockUser(userId: string) {
    console.log('Unblocking user with ID:', userId);
    if (!userId || userId === 'undefined') {
      console.error('Invalid user ID:', userId);
      return;
    }
    
    this.adminService.unblockUser(userId).subscribe({
      next: () => {
        console.log('User unblocked successfully');
        this.loadMaliciousUsers();
      },
      error: (error: any) => {
        console.error('Error unblocking user:', error);
      }
    });
  }

  updateProblemStatus(problemId: string, status: string) {
    console.log('Updating problem status:', { problemId, status });
    if (!problemId || problemId === 'undefined') {
      console.error('Invalid problem ID:', problemId);
      return;
    }
    
    this.adminService.updateProblemStatus(problemId, status).subscribe({
      next: () => {
        console.log('Problem status updated successfully');
        this.loadAllProblems();
      },
      error: (error: any) => {
        console.error('Error updating problem status:', error);
      }
    });
  }

  getStatusLabel(status: string): string {
    switch (status) {
      case 'Pending': return 'Na čekanju';
      case 'UnderReview': return 'Na reviziji';
      case 'Resolved': return 'Rešeno';
      case 'Rejected': return 'Odbacen';
      default: return status;
    }
  }

  getStatusColor(status: string): string {
    switch (status) {
      case 'Pending': return 'warn';
      case 'UnderReview': return 'accent';
      case 'Resolved': return 'primary';
      case 'Rejected': return 'warn';
      default: return 'primary';
    }
  }

  getAvailableStatusOptions(currentStatus: string): string[] {
    console.log('Getting available status options for:', currentStatus);
    switch (currentStatus) {
      case 'UnderReview':
        return ['Pending', 'Rejected', 'Resolved'];
      case 'Pending':
        // Admin može da vidi probleme na čekanju, ali ne može da ih menja
        // Vodič treba prvo da ih pošalje na reviziju
        return [];
      default:
        return [];
    }
  }

  getStatusTransitionLabel(currentStatus: string, newStatus: string): string {
    switch (newStatus) {
      case 'Pending':
        return 'Vrati na čekanje';
      case 'Rejected':
        return 'Odbaci problem';
      case 'Resolved':
        return 'Reši problem';
      default:
        return newStatus;
    }
  }

  getStatusButtonColor(status: string): string {
    switch (status) {
      case 'Pending':
        return 'primary';
      case 'Rejected':
        return 'warn';
      case 'Resolved':
        return 'accent';
      default:
        return 'primary';
    }
  }

  getStatusIcon(status: string): string {
    switch (status) {
      case 'Pending':
        return 'schedule';
      case 'Rejected':
        return 'close';
      case 'Resolved':
        return 'check';
      default:
        return 'help';
    }
  }


} 