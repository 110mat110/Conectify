import { TestBed } from '@angular/core/testing';

import { RuleProviderService } from './rule-provider.service';

describe('RuleProviderService', () => {
  let service: RuleProviderService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RuleProviderService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
