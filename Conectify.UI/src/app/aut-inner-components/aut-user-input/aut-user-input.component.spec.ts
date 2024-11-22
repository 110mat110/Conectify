import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AutUserInputComponent } from './aut-user-input.component';

describe('AutUserInputComponent', () => {
  let component: AutUserInputComponent;
  let fixture: ComponentFixture<AutUserInputComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AutUserInputComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AutUserInputComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
