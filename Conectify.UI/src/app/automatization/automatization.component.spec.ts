import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AutomatizationComponent } from './automatization.component';

describe('AutomatizationComponent', () => {
  let component: AutomatizationComponent;
  let fixture: ComponentFixture<AutomatizationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AutomatizationComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AutomatizationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
