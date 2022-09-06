import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AutomatizationComponentComponent } from './automatization-component.component';

describe('AutomatizationComponentComponent', () => {
  let component: AutomatizationComponentComponent;
  let fixture: ComponentFixture<AutomatizationComponentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AutomatizationComponentComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AutomatizationComponentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
