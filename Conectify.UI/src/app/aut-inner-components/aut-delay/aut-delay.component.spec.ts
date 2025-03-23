import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AutDelayComponent } from './aut-delay.component';

describe('AutDelayComponent', () => {
  let component: AutDelayComponent;
  let fixture: ComponentFixture<AutDelayComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AutDelayComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AutDelayComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
