import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';

export interface User {
  Id: string;
  Username: string;
  Email: string;
  FirstName: string;
  LastName: string;
  Role: string;
  Interests: string[];
  BonusPoints: number;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  interests: number[];
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  Token: string;
  Username: string;
  Role: string;
  Id: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private tokenKey = 'token';
  private userKey = 'user';

  constructor(
    private http: HttpClient,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {}

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/api/Users/login`, credentials);
  }

  register(data: RegisterRequest): Observable<any> {
    return this.http.post(`${environment.apiUrl}/api/Users/register`, data);
  }

  setToken(token: string) {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(this.tokenKey, token);
    }
  }

  getToken(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      return localStorage.getItem(this.tokenKey);
    }
    return null;
  }

  setUser(user: any) {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(this.userKey, JSON.stringify(user));
    }
  }

  getUser(): any {
    if (isPlatformBrowser(this.platformId)) {
      const user = localStorage.getItem(this.userKey);
      return user ? JSON.parse(user) : null;
    }
    return null;
  }

  logout() {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem(this.tokenKey);
      localStorage.removeItem(this.userKey);
    }
    // Redirect to login page
    this.router.navigate(['/login']);
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  loadUserFromStorage() {
    if (isPlatformBrowser(this.platformId)) {
      const token = localStorage.getItem(this.tokenKey);
      const user = localStorage.getItem(this.userKey);
      return { token, user: user ? JSON.parse(user) : null };
    }
    return { token: null, user: null };
  }
} 