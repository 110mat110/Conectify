import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AutHttpCallComponent } from './aut-http-call.component';

describe('AutHttpCallComponent', () => {
  let component: AutHttpCallComponent;
  let fixture: ComponentFixture<AutHttpCallComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AutHttpCallComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AutHttpCallComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
