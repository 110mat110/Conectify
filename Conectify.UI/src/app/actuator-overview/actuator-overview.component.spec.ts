import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ActuatorOverviewComponent } from './actuator-overview.component';

describe('ActuatorOverviewComponent', () => {
  let component: ActuatorOverviewComponent;
  let fixture: ComponentFixture<ActuatorOverviewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ActuatorOverviewComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ActuatorOverviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
