import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ActuatorCubeComponent } from './actuator-cube.component';

describe('ActuatorCubeComponent', () => {
  let component: ActuatorCubeComponent;
  let fixture: ComponentFixture<ActuatorCubeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ActuatorCubeComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ActuatorCubeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
