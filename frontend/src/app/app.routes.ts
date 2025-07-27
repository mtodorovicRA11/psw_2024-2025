import { Routes } from '@angular/router';
import { LoginComponent } from './components/auth/login/login.component';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  // TODO: Add more routes for tours, admin, guide pages
];
