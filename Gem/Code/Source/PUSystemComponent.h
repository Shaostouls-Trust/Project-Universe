#pragma once

#include <AzCore/Component/Component.h>

#include <PU/PUBus.h>

namespace PU
{
    class PUSystemComponent
        : public AZ::Component
        , protected PURequestBus::Handler
    {
    public:
        AZ_COMPONENT(PUSystemComponent, "{C7482EC8-8F07-4DA1-B22E-DFA0D6C72FF8}");

        static void Reflect(AZ::ReflectContext* context);

        static void GetProvidedServices(AZ::ComponentDescriptor::DependencyArrayType& provided);
        static void GetIncompatibleServices(AZ::ComponentDescriptor::DependencyArrayType& incompatible);
        static void GetRequiredServices(AZ::ComponentDescriptor::DependencyArrayType& required);
        static void GetDependentServices(AZ::ComponentDescriptor::DependencyArrayType& dependent);

    protected:
        ////////////////////////////////////////////////////////////////////////
        // PURequestBus interface implementation

        ////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////
        // AZ::Component interface implementation
        void Init() override;
        void Activate() override;
        void Deactivate() override;
        ////////////////////////////////////////////////////////////////////////
    };
}
