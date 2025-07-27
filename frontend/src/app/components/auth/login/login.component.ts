import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  credentials = {
    username: '',
    password: ''
  };
  errorMessage = '';
  isLoading = false;

  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  onLogin() {
    this.isLoading = true;
    this.errorMessage = '';

    this.authService.login(this.credentials).subscribe({
      next: (response) => {
        this.isLoading = false;
        console.log('Login successful:', response);
        // Navigate based on user role
        if (response.role === 'Admin') {
          this.router.navigate(['/admin']);
        } else if (response.role === 'Guide') {
          this.router.navigate(['/guide']);
        } else {
          this.router.navigate(['/tours']);
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.error?.error || 'Greška pri prijavi';
        console.error('Login error:', error);
      }
    });
  }
} 