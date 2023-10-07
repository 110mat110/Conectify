import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AutEachTimeComponent } from './aut-each-time.component';

describe('AutEachTimeComponent', () => {
  let component: AutEachTimeComponent;
  let fixture: ComponentFixture<AutEachTimeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AutEachTimeComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AutEachTimeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
