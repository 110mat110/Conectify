import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AutomatizationEditComponent } from './automatization-edit.component';

describe('AutomatizationEditComponent', () => {
  let component: AutomatizationEditComponent;
  let fixture: ComponentFixture<AutomatizationEditComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AutomatizationEditComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AutomatizationEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
