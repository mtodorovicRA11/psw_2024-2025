# Map Implementation - TourApp

## Overview
TourApp sada koristi **Leaflet** biblioteku za prikaz interaktivne mape sa markerima i rutama tura.

## Features

### 🗺️ **Interaktivna Mapa**
- **OpenStreetMap** kao osnovna mapa
- **Marker-i** za svaku ključnu tačku ture
- **Rute** koje povezuju tačke ture
- **Legenda** sa bojama za različite ture
- **Popup-ovi** sa detaljnim informacijama o tačkama

### 🎨 **Vizuelni Elementi**
- **Obojeni marker-i** - svaka tura ima svoju boju
- **Numerisani marker-i** - prikazuju redosled tačaka (1, 2, 3...)
- **Linije ruta** - povezuju tačke ture
- **Oznake ruta** - prikazuju ime ture na sredini rute
- **Legenda** - desno gore sa svim turama i njihovim bojama

### 📱 **Responsive Design**
- Mapa se prilagođava veličini ekrana
- Legenda se pozicionira na desno gore
- Popup-ovi su stilizovani u skladu sa aplikacijom

## Technical Implementation

### Dependencies
```json
{
  "leaflet": "^1.9.4",
  "@types/leaflet": "^1.9.8"
}
```

### CSS Integration
```html
<!-- Leaflet CSS -->
<link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
```

### Component Structure
```typescript
// MapComponent
export interface MapPoint {
  id: string;
  name: string;
  description: string;
  latitude: number;
  longitude: number;
}

export interface TourRoute {
  tourId: string;
  tourName: string;
  points: MapPoint[];
  color?: string;
}
```

### Key Methods
- `initMap()` - Inicijalizuje Leaflet mapu
- `renderTours()` - Prikazuje sve ture na mapi
- `addTourToMap()` - Dodaje pojedinačnu turu
- `createMarker()` - Kreira marker sa popup-om
- `getTourColor()` - Dodeljuje boju za turu

## Usage

### Tourist Dashboard
```typescript
// Automatski se učitavaju podaci za mapu
loadTours() {
  this.tourService.getPublishedTours().subscribe(tours => {
    this.tours = tours;
    this.loadTourRoutes(); // Konvertuje ture u rute za mapu
  });
}

// Toggle mapa
toggleMap() {
  this.showMap = !this.showMap;
  if (this.showMap) {
    this.loadTourRoutes();
  }
}
```

### Guide Dashboard
```typescript
// Isti princip kao za turiste
loadTours() {
  this.tourService.getMyTours().subscribe(tours => {
    this.tours = tours;
    this.loadTourRoutes();
  });
}
```

## Map Features

### 🎯 **Markeri**
- Kružni marker-i sa brojevima (1, 2, 3...)
- Boja marker-a odgovara boji ture
- Klik na marker otvara popup sa detaljima

### 🛣️ **Rute**
- Linije koje povezuju tačke ture
- Boja linije odgovara boji ture
- Oznaka sa imenom ture na sredini rute

### 📋 **Popup Sadržaj**
- Ime tačke
- Opis tačke
- Ime ture
- Redni broj tačke
- Koordinate

### 🎨 **Legenda**
- Pozicionirana desno gore
- Prikazuje sve ture sa njihovim bojama
- Automatski se ažurira kada se dodaju nove ture

## Color Scheme
```typescript
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
```

## SSR Compatibility
- Mapa se inicijalizuje samo u browser-u
- `isPlatformBrowser()` provera sprečava SSR greške
- Leaflet je dodan u `allowedCommonJsDependencies`

## Test Data
Backend endpoint `/api/Tour/create-test-tours` kreira test ture sa ključnim tačkama u Beogradu:

### Demo Tura
- **Kalemegdan** (44.8235, 20.4499)
- **Knez Mihailova** (44.8178, 20.4594)
- **Skadarlija** (44.8194, 20.4656)
- **Hram Svetog Save** (44.7980, 20.4689)

### Gradska Tura
- **Trg Republike** (44.8168, 20.4594)
- **Muzej Nikole Tesle** (44.8038, 20.4489)
- **Botanička Bašta Jevremovac** (44.8189, 20.4756)

## Troubleshooting

### Mapa se ne prikazuje
1. Proverite da li je Leaflet instaliran: `npm install leaflet @types/leaflet`
2. Proverite da li je Leaflet CSS uključen u `index.html`
3. Proverite konzolu za greške

### Markeri se ne prikazuju
1. Proverite da li ture imaju `keyPoints` sa `latitude` i `longitude`
2. Proverite da li su koordinate validne (između -90 i 90 za lat, -180 i 180 za lng)

### SSR Greške
1. Proverite da li je Leaflet u `allowedCommonJsDependencies` u `angular.json`
2. Proverite da li se koristi `isPlatformBrowser()` provera

## Future Enhancements
- [ ] Dodavanje custom ikona za različite tipove tačaka
- [ ] Animacije za rute
- [ ] Clustering za veliki broj marker-a
- [ ] Offline mapa podrška
- [ ] 3D prikaz sa elevation podacima 