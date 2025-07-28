import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';
import { AuthService, LoginResponse } from '../../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatIconModule
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
        
        // Save token and user data
        this.authService.setToken(response.Token);
        this.authService.setUser({
          id: response.Id,
          username: response.Username,
          role: response.Role
        });
        
        // Navigate based on user role
        if (response.Role === 'Admin') {
          this.router.navigate(['/admin']);
        } else if (response.Role === 'Guide') {
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