import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-register',
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
    MatSelectModule,
    MatCheckboxModule,
    MatIconModule
  ],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
  userData = {
    username: '',
    password: '',
    firstName: '',
    lastName: '',
    email: '',
    interests: [] as string[]
  };

  availableInterests = [
    { value: 'Nature', label: 'Priroda' },
    { value: 'Art', label: 'Umetnost' },
    { value: 'Sport', label: 'Sport' },
    { value: 'Shopping', label: 'Kupovina' },
    { value: 'Food', label: 'Hrana' }
  ];

  errorMessage = '';
  isLoading = false;

  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  onInterestChange(interest: string, checked: boolean) {
    if (checked) {
      this.userData.interests.push(interest);
    } else {
      const index = this.userData.interests.indexOf(interest);
      if (index > -1) {
        this.userData.interests.splice(index, 1);
      }
    }
  }

  onRegister() {
    this.isLoading = true;
    this.errorMessage = '';

    // Konvertuj stringove u enum vrednosti
    const requestData = {
      username: this.userData.username,
      password: this.userData.password,
      firstName: this.userData.firstName,
      lastName: this.userData.lastName,
      email: this.userData.email,
      interests: this.userData.interests.map(interest => {
        switch(interest) {
          case 'Nature': return 0;
          case 'Art': return 1;
          case 'Sport': return 2;
          case 'Shopping': return 3;
          case 'Food': return 4;
          default: return 0;
        }
      })
    };

    this.authService.register(requestData).subscribe({
      next: (response) => {
        this.isLoading = false;
        console.log('Registration successful:', response);
        this.router.navigate(['/login']);
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.error?.error || 'Greška pri registraciji';
        console.error('Registration error:', error);
      }
    });
  }
} 