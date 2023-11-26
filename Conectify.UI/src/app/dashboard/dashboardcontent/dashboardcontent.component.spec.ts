import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DashboardcontentComponent } from './dashboardcontent.component';

describe('DashboardcontentComponent', () => {
  let component: DashboardcontentComponent;
  let fixture: ComponentFixture<DashboardcontentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DashboardcontentComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DashboardcontentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
