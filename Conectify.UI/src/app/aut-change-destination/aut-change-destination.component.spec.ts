import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AutChangeDestinationComponent } from './aut-change-destination.component';

describe('AutChangeDestinationComponent', () => {
  let component: AutChangeDestinationComponent;
  let fixture: ComponentFixture<AutChangeDestinationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AutChangeDestinationComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AutChangeDestinationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
