import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SelectInputSensorOverlayComponent } from './select-input-sensor-overlay.component';

describe('SelectInputSensorOverlayComponent', () => {
  let component: SelectInputSensorOverlayComponent;
  let fixture: ComponentFixture<SelectInputSensorOverlayComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SelectInputSensorOverlayComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SelectInputSensorOverlayComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
