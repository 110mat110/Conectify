import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ActuatorOverviewComponent } from './actuator-overview/actuator-overview.component';
import { AutomatizationComponent } from './automatization/automatization.component';
import { MetadataComponent } from './metadata/metadata.component';
import { SensorOverviewComponent } from './sensor-overview/sensor-overview.component';
import { DashboardComponent } from './dashboard/dashboard/dashboard.component';
import { LoadingComponent } from './dashboard/loading/loading.component';
import { DeviceComponent } from './device/device.component';

const routes: Routes = [
  {path: 'sensors', component: SensorOverviewComponent},
  {path: 'actuators', component: ActuatorOverviewComponent},
  {path: 'automatization', component: AutomatizationComponent},
  {path: 'metadata', component: MetadataComponent},
  {path: 'dashboard', component: DashboardComponent},
  {path: 'loading', component: LoadingComponent},
  {path: 'device/:id', component: DeviceComponent},
  { path: '', redirectTo: '/loading', pathMatch: 'full' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
