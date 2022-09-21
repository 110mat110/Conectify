import { APP_INITIALIZER, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { SensorOverviewComponent } from './sensor-overview/sensor-overview.component';
import { HttpBackend, HttpClientModule } from '@angular/common/http';
import { DebugMessagesComponent } from './debug-messages/debug-messages.component';
import { SensorCubeComponent } from './sensor-cube/sensor-cube.component';
import { ActuatorCubeComponent } from './actuator-cube/actuator-cube.component';
import { ActuatorOverviewComponent } from './actuator-overview/actuator-overview.component';
import { SensorDetailComponent } from './sensor-detail/sensor-detail.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatSliderModule } from '@angular/material/slider';
import {MatGridListModule} from '@angular/material/grid-list'; 
import * as echarts from 'echarts';
import { NgxEchartsModule } from 'ngx-echarts';
import {OverlayModule} from '@angular/cdk/overlay';
import {MatInputModule} from '@angular/material/input'; 
import {MatIconModule} from '@angular/material/icon';
import { HashLocationStrategy, LocationStrategy } from '@angular/common';
import { MetadataIconsComponent } from './metadata-icons/metadata-icons.component';
import { AutomatizationComponent } from './automatization/automatization.component';
import { AutValueInputComponent } from './aut-value-input/aut-value-input.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AutomatizationComponentComponent } from './automatization-component/automatization-component.component';
import { SelectInputSensorOverlayComponent } from './select-input-sensor-overlay/select-input-sensor-overlay.component';
import {DragDropModule} from '@angular/cdk/drag-drop';
import { AutChangeDestinationComponent } from './aut-change-destination/aut-change-destination.component';
import { SelectDestinationActuatorOverlayComponent } from './select-destination-actuator-overlay/select-destination-actuator-overlay.component';
import { MetadataComponent } from './metadata/metadata.component';
import {MatListModule} from '@angular/material/list';
import {MatButtonModule} from '@angular/material/button';
import {MatSelectModule} from '@angular/material/select';

@NgModule({
  declarations: [
    AppComponent,
    SensorOverviewComponent,
    DebugMessagesComponent,
    SensorCubeComponent,
    ActuatorCubeComponent,
    ActuatorOverviewComponent,
    SensorDetailComponent,
    MetadataIconsComponent,
    AutomatizationComponent,
    AutValueInputComponent,
    AutomatizationComponentComponent,
    SelectInputSensorOverlayComponent,
    AutChangeDestinationComponent,
    SelectDestinationActuatorOverlayComponent,
    MetadataComponent
  ],
  imports: [
    FormsModule,
    DragDropModule,
    BrowserModule,
    MatSliderModule,
    MatGridListModule,
    AppRoutingModule,
    HttpClientModule,
    ReactiveFormsModule,
    BrowserAnimationsModule,
    NgxEchartsModule.forRoot({
      echarts
    }),
    OverlayModule,
    MatInputModule,
    MatIconModule,
    MatListModule,
    MatButtonModule,
    MatSelectModule
  ],  providers: [{provide: LocationStrategy, useClass: HashLocationStrategy}],
  bootstrap: [AppComponent]
})
export class AppModule { }
