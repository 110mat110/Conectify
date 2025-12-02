import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AppsOverviewComponent } from './apps-overview.component';

describe('AppsOverviewComponent', () => {
  let component: AppsOverviewComponent;
  let fixture: ComponentFixture<AppsOverviewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppsOverviewComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AppsOverviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
