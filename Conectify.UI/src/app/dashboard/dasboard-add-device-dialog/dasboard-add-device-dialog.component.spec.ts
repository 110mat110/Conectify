import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DasboardAddDeviceDialogComponent } from './dasboard-add-device-dialog.component';

describe('DasboardAddDeviceDialogComponent', () => {
  let component: DasboardAddDeviceDialogComponent;
  let fixture: ComponentFixture<DasboardAddDeviceDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DasboardAddDeviceDialogComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DasboardAddDeviceDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
