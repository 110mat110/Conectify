import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LastValueComponent } from './last-value.component';

describe('LastValueComponent', () => {
  let component: LastValueComponent;
  let fixture: ComponentFixture<LastValueComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ LastValueComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(LastValueComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
