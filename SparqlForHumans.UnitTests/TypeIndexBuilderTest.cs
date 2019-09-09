using Xunit;

namespace SparqlForHumans.UnitTests
{
    /// <summary>
    ///     Given the following nTriples file:
    ///     Q76 (Obama) -> P31 (InstanceOf) -> Q5 (Human)
    ///     Q76 (Obama) -> P27 -> Qxx
    ///     Q76 (Obama) -> P555 -> Qxx
    ///     ...
    ///     Q77 (Other Human) -> P31 (InstanceOf) -> Q5 (Human)
    ///     Q77 (Other Human) -> P33 -> Qxx
    ///     Q77 (Other Human) -> P44 -> Qxx
    ///     ...
    ///     Q5 (Human)
    ///     ...
    ///     Q278 (Chile) -> P31 (InstanceOf) -> Q17 (Country)
    ///     Q278 (Chile) -> P555 -> Qxx
    ///     Q278 (Chile) -> P777 -> Qxx
    ///     ...
    ///     Q17 (Country)
    ///     ...
    ///     Should return a dictionary with the following:
    ///     (Human) Q5 (Obama + OtherHuman): P31, P27, P555, P33, P44
    ///     (Country) Q17 (Chile): P31, P555, P777
    /// </summary>
}