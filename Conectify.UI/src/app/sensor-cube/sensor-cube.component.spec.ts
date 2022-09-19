import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SensorCubeComponent } from './sensor-cube.component';

describe('SensorCubeComponent', () => {
  let component: SensorCubeComponent;
  let fixture: ComponentFixture<SensorCubeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SensorCubeComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SensorCubeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
