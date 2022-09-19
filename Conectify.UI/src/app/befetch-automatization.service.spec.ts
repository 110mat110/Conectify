import { TestBed } from '@angular/core/testing';

import { BefetchAutomatizationService } from './befetch-automatization.service';

describe('BefetchAutomatizationService', () => {
  let service: BefetchAutomatizationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BefetchAutomatizationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
