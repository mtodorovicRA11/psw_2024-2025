import { Routes } from '@angular/router';
import { LoginComponent } from './components/auth/login/login.component';
import { RegisterComponent } from './components/auth/register/register.component';
import { TouristDashboardComponent } from './components/tourist/tourist-dashboard/tourist-dashboard.component';
import { GuideDashboardComponent } from './components/guide/guide-dashboard/guide-dashboard.component';
import { AdminDashboardComponent } from './components/admin/admin-dashboard/admin-dashboard.component';
import { AuthGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'tours', component: TouristDashboardComponent, canActivate: [AuthGuard] },
  { path: 'guide', component: GuideDashboardComponent, canActivate: [AuthGuard] },
  { path: 'admin', component: AdminDashboardComponent, canActivate: [AuthGuard] }
];
