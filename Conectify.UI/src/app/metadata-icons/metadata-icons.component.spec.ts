import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MetadataIconsComponent } from './metadata-icons.component';

describe('MetadataIconsComponent', () => {
  let component: MetadataIconsComponent;
  let fixture: ComponentFixture<MetadataIconsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MetadataIconsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(MetadataIconsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
