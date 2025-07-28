import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { User } from './auth.service';

export interface Problem {
  Id: string;
  TourId: string;
  TourName: string;
  TouristId: string;
  TouristName: string;
  Title: string;
  Description: string;
  Status: string;
  CreatedAt: string;
  UpdatedAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  constructor(private http: HttpClient) {}

  getMaliciousUsers(): Observable<User[]> {
    return this.http.get<User[]>(`${environment.apiUrl}/api/Users/malicious`);
  }

  blockUser(userId: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/api/Users/block/${userId}`, {});
  }

  unblockUser(userId: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/api/Users/unblock/${userId}`, {});
  }

  getAllProblems(): Observable<Problem[]> {
    return this.http.get<Problem[]>(`${environment.apiUrl}/api/Tour/problems/admin`);
  }

  updateProblemStatus(problemId: string, status: string): Observable<Problem> {
    return this.http.post<Problem>(`${environment.apiUrl}/api/Tour/update-problem-status`, {
      problemId,
      status
    });
  }
} 