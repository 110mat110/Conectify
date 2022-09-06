import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ActuatorOverviewComponent } from './actuator-overview/actuator-overview.component';
import { AutomatizationComponent } from './automatization/automatization.component';
import { SensorOverviewComponent } from './sensor-overview/sensor-overview.component';

const routes: Routes = [
  {path: 'sensors', component: SensorOverviewComponent},
  {path: 'actuators', component: ActuatorOverviewComponent},
  {path: 'automatization', component: AutomatizationComponent},
  { path: '', redirectTo: '/sensors', pathMatch: 'full' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
