import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AutAndComponent } from './aut-and.component';

describe('AutAndComponent', () => {
  let component: AutAndComponent;
  let fixture: ComponentFixture<AutAndComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AutAndComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AutAndComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
