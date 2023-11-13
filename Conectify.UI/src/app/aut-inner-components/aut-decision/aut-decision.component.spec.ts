import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AutDecisionComponent } from './aut-decision.component';

describe('AutDecisionComponent', () => {
  let component: AutDecisionComponent;
  let fixture: ComponentFixture<AutDecisionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AutDecisionComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AutDecisionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
