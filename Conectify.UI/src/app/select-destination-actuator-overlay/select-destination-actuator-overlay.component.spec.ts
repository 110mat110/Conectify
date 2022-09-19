import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SelectDestinationActuatorOverlayComponent } from './select-destination-actuator-overlay.component';

describe('SelectDestinationActuatorOverlayComponent', () => {
  let component: SelectDestinationActuatorOverlayComponent;
  let fixture: ComponentFixture<SelectDestinationActuatorOverlayComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SelectDestinationActuatorOverlayComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SelectDestinationActuatorOverlayComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
