import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Tour {
  Id: string;
  Name: string;
  Description: string;
  Difficulty: string;
  Category: string;
  Price: number;
  Date: string;
  State: string;
  GuideId: string;
  KeyPoints: KeyPoint[];
  isPublishing?: boolean;
  isCancelling?: boolean;
}

export interface KeyPoint {
  Id: string;
  Name: string;
  Description: string;
  Latitude: number;
  Longitude: number;
}

export interface CartItem {
  tourId: string;
  name: string;
  price: number;
  guideName: string;
  date: string;
  category: string;
}

export interface Cart {
  items: CartItem[];
  totalPrice: number;
  maxUsableBonusPoints: number;
}

export interface PurchaseRequest {
  tourIds: string[];
}

@Injectable({
  providedIn: 'root'
})
export class TourService {
  constructor(private http: HttpClient) {}

  getPublishedTours(category?: string | null, guideId?: string | null, onlyAwardedGuides?: boolean): Observable<Tour[]> {
    let params: any = {};
    if (category) params.category = category;
    if (guideId) params.guideId = guideId;
    if (onlyAwardedGuides) params.onlyAwardedGuides = onlyAwardedGuides;

    return this.http.get<Tour[]>(`${environment.apiUrl}/api/Tour/published`, { params });
  }

  getCart(): Observable<Cart> {
    return this.http.get<Cart>(`${environment.apiUrl}/api/Tour/cart`);
  }

  addToCart(tourId: string): Observable<void> {
    console.log('TourService: Adding to cart, tourId:', tourId);
    console.log('TourService: API URL:', `${environment.apiUrl}/api/Tour/cart/add/${tourId}`);
    return this.http.post<void>(`${environment.apiUrl}/api/Tour/cart/add/${tourId}`, {});
  }

  removeFromCart(tourId: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/api/Tour/cart/remove/${tourId}`, {});
  }

  purchaseTours(request: PurchaseRequest): Observable<any> {
    return this.http.post(`${environment.apiUrl}/api/Tour/purchase-multiple`, request);
  }

  purchaseSingleTour(tourId: string, useBonusPoints: number): Observable<any> {
    return this.http.post(`${environment.apiUrl}/api/Tour/purchase`, {
      tourId: tourId,
      useBonusPoints: useBonusPoints
    });
  }

  getGuideReport(year: number, month: number): Observable<any> {
    return this.http.get(`${environment.apiUrl}/api/Tour/guide-report?year=${year}&month=${month}`);
  }

  getMyTours(state?: string | null): Observable<Tour[]> {
    let params: any = {};
    if (state) params.state = state;
    return this.http.get<Tour[]>(`${environment.apiUrl}/api/Tour`, { params });
  }

  createTour(tourData: any): Observable<Tour> {
    return this.http.post<Tour>(`${environment.apiUrl}/api/Tour`, tourData);
  }

  addKeyPoint(keyPointData: any): Observable<any> {
    return this.http.post(`${environment.apiUrl}/api/Tour/keypoint`, keyPointData);
  }

  publishTour(tourId: string): Observable<Tour> {
    return this.http.post<Tour>(`${environment.apiUrl}/api/Tour/publish/${tourId}`, {});
  }

  cancelTour(tourId: string): Observable<Tour> {
    return this.http.post<Tour>(`${environment.apiUrl}/api/Tour/cancel/${tourId}`, {});
  }

  rateTour(ratingRequest: any): Observable<any> {
    return this.http.post(`${environment.apiUrl}/api/Tour/rate`, ratingRequest);
  }

  reportProblem(problemRequest: any): Observable<any> {
    return this.http.post(`${environment.apiUrl}/api/Tour/report-problem`, problemRequest);
  }

  getMyProblems(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/api/Tour/problems/tourist`);
  }

  getGuideProblems(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/api/Tour/problems/guide`);
  }

  updateProblemStatus(problemId: string, newStatus: string): Observable<any> {
    return this.http.post(`${environment.apiUrl}/api/Tour/update-problem-status`, {
      problemId: problemId,
      newStatus: newStatus
    });
  }

  getPurchasedTours(): Observable<Tour[]> {
    return this.http.get<Tour[]>(`${environment.apiUrl}/api/Tour/purchased`);
  }

  getProblemEvents(problemId: string): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/api/Tour/problems/${problemId}/events`);
  }

  getAllProblemEvents(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/api/Tour/problems/events`);
  }
} 