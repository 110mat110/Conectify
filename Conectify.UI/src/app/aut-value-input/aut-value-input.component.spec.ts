import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AutValueInputComponent } from './aut-value-input.component';

describe('AutValueInputComponent', () => {
  let component: AutValueInputComponent;
  let fixture: ComponentFixture<AutValueInputComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AutValueInputComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AutValueInputComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
