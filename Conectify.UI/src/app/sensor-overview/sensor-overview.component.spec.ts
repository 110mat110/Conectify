import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SensorOverviewComponent } from './sensor-overview.component';

describe('SensorOverviewComponent', () => {
  let component: SensorOverviewComponent;
  let fixture: ComponentFixture<SensorOverviewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SensorOverviewComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SensorOverviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
