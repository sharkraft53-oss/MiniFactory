using System;

namespace MiniFactory.Boost
{
    public class BoostService
    {
        private readonly bool _enabled;
        private readonly float _durationSeconds;
        private readonly float _multiplier;

        private DateTime _endTimeUtc =
            DateTime.MinValue;

        public BoostService(
            bool enabled,
            float durationSeconds,
            float multiplier)
        {
            _enabled = enabled;
            _durationSeconds = durationSeconds;
            _multiplier = multiplier;
        }

        public bool IsActive =>
            _enabled &&
            DateTime.UtcNow < _endTimeUtc;

        public float Multiplier =>
            IsActive
                ? _multiplier
                : 1f;

        public double RemainingSeconds
        {
            get
            {
                if (!IsActive)
                    return 0;

                return Math.Max(
                    0,
                    (_endTimeUtc -
                     DateTime.UtcNow)
                    .TotalSeconds
                );
            }
        }

        public long EndUnixTimeSeconds
        {
            get
            {
                if (_endTimeUtc ==
                    DateTime.MinValue)
                {
                    return 0;
                }

                return new DateTimeOffset(
                    _endTimeUtc
                ).ToUnixTimeSeconds();
            }
        }

        public bool TryStartBoost()
        {
            if (!_enabled)
                return false;

            if (IsActive)
                return false;

            _endTimeUtc =
                DateTime.UtcNow.AddSeconds(
                    _durationSeconds
                );

            return true;
        }

        public void RestoreEndTime(
            long endUnixTimeSeconds)
        {
            if (endUnixTimeSeconds <= 0)
            {
                _endTimeUtc =
                    DateTime.MinValue;

                return;
            }

            _endTimeUtc =
                DateTimeOffset
                    .FromUnixTimeSeconds(
                        endUnixTimeSeconds
                    )
                    .UtcDateTime;
        }
    }
}