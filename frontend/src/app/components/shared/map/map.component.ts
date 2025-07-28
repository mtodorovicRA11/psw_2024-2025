import { Component, Input, OnInit, OnDestroy, ElementRef, ViewChild, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';

// Leaflet type declaration
declare var L: any;

export interface MapPoint {
  id: string;
  name: string;
  description: string;
  latitude: number;
  longitude: number;
  imageUrl?: string;
}

export interface TourRoute {
  tourId: string;
  tourName: string;
  points: MapPoint[];
  color?: string;
}

@Component({
  selector: 'app-map',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="map-container">
      <div #mapContainer class="map-element"></div>
      <div class="map-legend" *ngIf="tours.length > 0">
        <h4>Legenda</h4>
        <div class="legend-item" *ngFor="let tour of tours; let i = index">
          <div class="legend-color" [style.background-color]="getTourColor(i)"></div>
          <span>{{ tour.tourName }}</span>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .map-container {
      position: relative;
      width: 100%;
      height: 500px;
      border-radius: 12px;
      overflow: hidden;
      box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
    }
    
    .map-element {
      width: 100%;
      height: 100%;
      z-index: 1;
    }
    
    .map-legend {
      position: absolute;
      top: 10px;
      right: 10px;
      background: rgba(255, 255, 255, 0.95);
      backdrop-filter: blur(10px);
      border-radius: 8px;
      padding: 15px;
      box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
      z-index: 1000;
      max-width: 250px;
    }
    
    .map-legend h4 {
      margin: 0 0 10px 0;
      font-size: 14px;
      color: #333;
      font-weight: 600;
    }
    
    .legend-item {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-bottom: 5px;
      font-size: 12px;
    }
    
    .legend-color {
      width: 16px;
      height: 16px;
      border-radius: 50%;
      border: 2px solid rgba(255, 255, 255, 0.8);
      box-shadow: 0 1px 3px rgba(0, 0, 0, 0.2);
    }
    
    .legend-item:last-child {
      margin-bottom: 0;
    }
  `]
})
export class MapComponent implements OnInit, OnDestroy {
  @Input() tours: TourRoute[] = [];
  @Input() center: { lat: number; lng: number } = { lat: 44.7866, lng: 20.4489 }; // Belgrade
  @Input() zoom: number = 12;
  @Input() showRoutes: boolean = true;
  @Input() showMarkers: boolean = true;

  @ViewChild('mapContainer', { static: true }) mapContainer!: ElementRef;

  private map: any;
  private markers: any[] = [];
  private routes: any[] = [];
  private L: any;

  constructor(@Inject(PLATFORM_ID) private platformId: Object) {}

  async ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      await this.initMap();
    }
  }

  ngOnDestroy() {
    if (this.map) {
      this.map.remove();
    }
  }

  private async initMap() {
    if (!isPlatformBrowser(this.platformId) || !this.mapContainer) {
      return;
    }

    try {
      // Dynamically import Leaflet
      const L = await import('leaflet');
      
      // Initialize map
      this.map = L.map(this.mapContainer.nativeElement).setView([this.center.lat, this.center.lng], this.zoom);

      // Add OpenStreetMap tiles
      L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors',
        maxZoom: 18
      }).addTo(this.map);

      // Store L for later use
      this.L = L;

      // Render initial tours
      this.renderTours();
    } catch (error) {
      console.error('Error initializing map:', error);
    }
  }

  private renderTours() {
    if (!this.map || !this.tours || !this.L) return;

    // Clear existing markers and routes
    this.clearMap();

    this.tours.forEach((tour, index) => {
      this.addTourToMap(tour, index);
    });
  }

  private addTourToMap(tour: TourRoute, index: number) {
    if (!this.map || !tour.points || tour.points.length === 0 || !this.L) return;

    const color = this.getTourColor(index);
    const tourPoints = tour.points.map(point => [point.latitude, point.longitude]);

    // Add route line
    if (this.showRoutes && tourPoints.length > 1) {
      const route = this.L.polyline(tourPoints, {
        color: color,
        weight: 4,
        opacity: 0.8
      }).addTo(this.map);

      // Add route label
      const midPoint = tourPoints[Math.floor(tourPoints.length / 2)];
      const routeLabel = this.L.divIcon({
        className: 'route-label',
        html: `<div style="background: ${color}; color: white; padding: 2px 6px; border-radius: 4px; font-size: 10px; font-weight: bold;">${tour.tourName}</div>`,
        iconSize: [100, 20],
        iconAnchor: [50, 10]
      });

      this.L.marker(midPoint, { icon: routeLabel }).addTo(this.map);
      this.routes.push(route);
    }

    // Add markers for each point
    if (this.showMarkers) {
      tour.points.forEach((point, pointIndex) => {
        const marker = this.createMarker(point, tour, pointIndex, color);
        this.markers.push(marker);
      });
    }

    // Fit map to show all points
    if (tourPoints.length > 0) {
      const bounds = this.L.latLngBounds(tourPoints);
      this.map.fitBounds(bounds, { padding: [20, 20] });
    }
  }

  private createMarker(point: MapPoint, tour: TourRoute, pointIndex: number, color: string): any {
    if (!this.L) return null;

    const markerIcon = this.L.divIcon({
      className: 'custom-marker',
      html: `<div style="background: ${color}; width: 20px; height: 20px; border-radius: 50%; border: 3px solid white; box-shadow: 0 2px 6px rgba(0,0,0,0.3); display: flex; align-items: center; justify-content: center; color: white; font-weight: bold; font-size: 12px;">${pointIndex + 1}</div>`,
      iconSize: [20, 20],
      iconAnchor: [10, 10]
    });

    const marker = this.L.marker([point.latitude, point.longitude], { icon: markerIcon }).addTo(this.map);

    // Add popup with point information
    const popupContent = `
      <div style="min-width: 200px;">
        <h4 style="margin: 0 0 8px 0; color: #1976d2;">${point.name}</h4>
        <p style="margin: 0 0 8px 0; font-size: 14px;">${point.description}</p>
        <div style="font-size: 12px; color: #666;">
          <strong>Turа:</strong> ${tour.tourName}<br>
          <strong>Tačka:</strong> ${pointIndex + 1} od ${tour.points.length}<br>
          <strong>Koordinati:</strong> ${point.latitude.toFixed(4)}, ${point.longitude.toFixed(4)}
        </div>
      </div>
    `;

    marker.bindPopup(popupContent);
    return marker;
  }

  private clearMap() {
    // Remove markers
    this.markers.forEach(marker => {
      if (marker && this.map) {
        this.map.removeLayer(marker);
      }
    });
    this.markers = [];

    // Remove routes
    this.routes.forEach(route => {
      if (route && this.map) {
        this.map.removeLayer(route);
      }
    });
    this.routes = [];
  }

  public getTourColor(index: number): string {
    const colors = [
      '#1976d2', // Blue
      '#d32f2f', // Red
      '#388e3c', // Green
      '#f57c00', // Orange
      '#7b1fa2', // Purple
      '#c2185b', // Pink
      '#00796b', // Teal
      '#5d4037', // Brown
      '#455a64', // Blue Grey
      '#ff6f00'  // Deep Orange
    ];
    return colors[index % colors.length];
  }

  public updateTours(tours: TourRoute[]) {
    this.tours = tours;
    if (isPlatformBrowser(this.platformId) && this.map && this.L) {
      setTimeout(() => this.renderTours(), 100);
    }
  }

  public addTour(tour: TourRoute) {
    this.tours.push(tour);
    if (isPlatformBrowser(this.platformId) && this.map && this.L) {
      setTimeout(() => this.addTourToMap(tour, this.tours.length - 1), 100);
    }
  }

  public clearTours() {
    this.tours = [];
    if (isPlatformBrowser(this.platformId) && this.map && this.L) {
      this.clearMap();
    }
  }
} 