import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  username: string;
  role: string;
}

export interface User {
  id: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  interests: string[];
  bonusPoints: number;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadUserFromStorage();
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/api/Users/login`, credentials)
      .pipe(
        tap(response => {
          localStorage.setItem('token', response.token);
          this.setCurrentUser(response);
        })
      );
  }

  logout(): void {
    localStorage.removeItem('token');
    this.currentUserSubject.next(null);
  }

  isAuthenticated(): boolean {
    return !!localStorage.getItem('token');
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  private setCurrentUser(loginResponse: LoginResponse): void {
    // For now, create a basic user object from login response
    // In a real app, you might want to fetch full user details
    const user: User = {
      id: '', // Would come from JWT token or separate API call
      username: loginResponse.username,
      email: '',
      firstName: '',
      lastName: '',
      role: loginResponse.role,
      interests: [],
      bonusPoints: 0
    };
    this.currentUserSubject.next(user);
  }

  private loadUserFromStorage(): void {
    const token = this.getToken();
    if (token) {
      // In a real app, you might decode the JWT token here
      // or make an API call to get current user details
    }
  }
} 