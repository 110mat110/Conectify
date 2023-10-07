import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AutSetTimeComponent } from './aut-set-time.component';

describe('AutSetTimeComponent', () => {
  let component: AutSetTimeComponent;
  let fixture: ComponentFixture<AutSetTimeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AutSetTimeComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AutSetTimeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
