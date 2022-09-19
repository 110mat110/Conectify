import { TestBed } from '@angular/core/testing';

import { BEFetcherService } from './befetcher.service';

describe('BEFetcherService', () => {
  let service: BEFetcherService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BEFetcherService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
