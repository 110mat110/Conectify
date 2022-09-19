import { TestBed } from '@angular/core/testing';

import { MetadataIconService } from './metadata-icon.service';

describe('MetadataIconService', () => {
  let service: MetadataIconService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MetadataIconService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
