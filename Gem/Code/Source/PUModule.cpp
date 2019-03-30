
#include <AzCore/Memory/SystemAllocator.h>
#include <AzCore/Module/Module.h>

#include <PUSystemComponent.h>

namespace PU
{
    class PUModule
        : public AZ::Module
    {
    public:
        AZ_RTTI(PUModule, "{6E023E1E-3F68-470B-B7AE-C80E76F26D09}", AZ::Module);
        AZ_CLASS_ALLOCATOR(PUModule, AZ::SystemAllocator, 0);

        PUModule()
            : AZ::Module()
        {
            // Push results of [MyComponent]::CreateDescriptor() into m_descriptors here.
            m_descriptors.insert(m_descriptors.end(), {
                PUSystemComponent::CreateDescriptor(),
            });
        }

        /**
         * Add required SystemComponents to the SystemEntity.
         */
        AZ::ComponentTypeList GetRequiredSystemComponents() const override
        {
            return AZ::ComponentTypeList{
                azrtti_typeid<PUSystemComponent>(),
            };
        }
    };
}

// DO NOT MODIFY THIS LINE UNLESS YOU RENAME THE GEM
// The first parameter should be GemName_GemIdLower
// The second should be the fully qualified name of the class above
AZ_DECLARE_MODULE_CLASS(PU_f88d2c2e247e45358bc16b052c6aa108, PU::PUModule)
