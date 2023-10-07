import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AutSetValueComponent } from './aut-set-value.component';

describe('AutSetValueComponent', () => {
  let component: AutSetValueComponent;
  let fixture: ComponentFixture<AutSetValueComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AutSetValueComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AutSetValueComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
