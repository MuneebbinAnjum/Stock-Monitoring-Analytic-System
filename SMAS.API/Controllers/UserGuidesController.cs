using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMAS.API.DTOs;

namespace SMAS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class UserGuidesController : ControllerBase
    {
        [HttpGet("admin")]
        public IActionResult GetAdminGuide()
        {
            var guide = new UserGuideDto
            {
                Role = "Admin",
                Title = "Admin Portal Guide",
                Description = "Welcome to the SMAS Admin Portal",
                Features = new List<FeatureGuideDto>
                {
                    new FeatureGuideDto
                    {
                        Name = "Employee Management",
                        Description = "Manage salesman registrations, approvals, and rejections",
                        SubFeatures = new List<string>
                        {
                            "Approve/Reject new salesman registrations",
                            "Set monthly salary for salesmen",
                            "View employee performance metrics"
                        }
                    },
                    new FeatureGuideDto
                    {
                        Name = "Commission Management",
                        Description = "Set and manage salesman commissions",
                        SubFeatures = new List<string>
                        {
                            "Set commission percentage per product per salesman",
                            "View total commission earned by each salesman",
                            "View monthly commission summaries"
                        }
                    },
                    new FeatureGuideDto
                    {
                        Name = "Discount Management",
                        Description = "Create and manage product discounts",
                        SubFeatures = new List<string>
                        {
                            "Add percentage discounts on products",
                            "Set discount duration (start and end dates)",
                            "Edit or remove existing discounts",
                            "View active and inactive discounts"
                        }
                    },
                    new FeatureGuideDto
                    {
                        Name = "Order Management",
                        Description = "Manage customer and salesman orders",
                        SubFeatures = new List<string>
                        {
                            "View all orders in the system",
                            "Approve or reject pending orders",
                            "Track order status through fulfillment pipeline",
                            "Dispatch orders via courier",
                            "Update delivery information"
                        }
                    },
                    new FeatureGuideDto
                    {
                        Name = "Complaint Management",
                        Description = "View and resolve customer complaints",
                        SubFeatures = new List<string>
                        {
                            "View all customer complaints",
                            "Update complaint status",
                            "Add admin notes and responses",
                            "Approve or reject return requests",
                            "Automatically restore inventory for approved returns"
                        }
                    },
                    new FeatureGuideDto
                    {
                        Name = "Notifications",
                        Description = "Receive and manage system notifications",
                        SubFeatures = new List<string>
                        {
                            "View all system notifications",
                            "Access complete notification history",
                            "See unread notification count",
                            "Mark notifications as read"
                        }
                    },
                    new FeatureGuideDto
                    {
                        Name = "Account Management",
                        Description = "Manage your admin account",
                        SubFeatures = new List<string>
                        {
                            "Change your password",
                            "View your profile information",
                            "Access security settings"
                        }
                    }
                }
            };

            return Ok(new ApiResponse<UserGuideDto> { Success = true, Data = guide });
        }

        [HttpGet("salesman")]
        public IActionResult GetSalesmanGuide()
        {
            var guide = new UserGuideDto
            {
                Role = "Salesman",
                Title = "Salesman Portal Guide",
                Description = "Welcome to the SMAS Salesman Portal",
                Features = new List<FeatureGuideDto>
                {
                    new FeatureGuideDto
                    {
                        Name = "Account Approval",
                        Description = "Your account must be approved by an admin before you can login",
                        SubFeatures = new List<string>
                        {
                            "Wait for admin approval after registration",
                            "Status will show as 'Pending' until approved",
                            "You will receive notification once approved"
                        }
                    },
                    new FeatureGuideDto
                    {
                        Name = "Order Management",
                        Description = "Create and manage physical orders for customers",
                        SubFeatures = new List<string>
                        {
                            "Create new physical orders for customers",
                            "View your orders only",
                            "Track order status from creation to delivery",
                            "Add notes to orders"
                        }
                    },
                    new FeatureGuideDto
                    {
                        Name = "Commission Tracking",
                        Description = "View your commission earnings",
                        SubFeatures = new List<string>
                        {
                            "View commission percentage for each product",
                            "Track monthly commission earnings",
                            "View total compensation (salary + commission)",
                            "Check payment status"
                        }
                    },
                    new FeatureGuideDto
                    {
                        Name = "Product Catalog",
                        Description = "Browse products available for selling",
                        SubFeatures = new List<string>
                        {
                            "Search products by category",
                            "View product details and stock levels",
                            "See active discounts on products",
                            "Check current pricing"
                        }
                    },
                    new FeatureGuideDto
                    {
                        Name = "Notifications",
                        Description = "Stay updated with important events",
                        SubFeatures = new List<string>
                        {
                            "Receive notifications for new orders",
                            "Get notified of order status changes",
                            "View unread notification count",
                            "Mark notifications as read"
                        }
                    },
                    new FeatureGuideDto
                    {
                        Name = "Account Management",
                        Description = "Manage your salesman account",
                        SubFeatures = new List<string>
                        {
                            "Change your password",
                            "View your profile and sales target",
                            "Check your salary information"
                        }
                    }
                }
            };

            return Ok(new ApiResponse<UserGuideDto> { Success = true, Data = guide });
        }

        [HttpGet("buyer")]
        public IActionResult GetBuyerGuide()
        {
            var guide = new UserGuideDto
            {
                Role = "Buyer",
                Title = "Buyer Portal Guide",
                Description = "Welcome to the SMAS Buyer Portal",
                Features = new List<FeatureGuideDto>
                {
                    new FeatureGuideDto
                    {
                        Name = "Browse Products",
                        Description = "Explore and search for products",
                        SubFeatures = new List<string>
                        {
                            "Browse all available products",
                            "Filter by category",
                            "View product details and images",
                            "Check current pricing and discounts",
                            "See product availability"
                        }
                    },
                    new FeatureGuideDto
                    {
                        Name = "Shopping Cart",
                        Description = "Manage items you want to purchase",
                        SubFeatures = new List<string>
                        {
                            "Add products to your cart",
                            "Update quantities",
                            "Remove items from cart",
                            "View cart total with taxes and discounts"
                        }
                    },
                    new FeatureGuideDto
                    {
                        Name = "Wishlist",
                        Description = "Save items for later purchase",
                        SubFeatures = new List<string>
                        {
                            "Add products to wishlist",
                            "Move items from wishlist to cart",
                            "Remove items from wishlist",
                            "Track wishlist items for price changes"
                        }
                    },
                    new FeatureGuideDto
                    {
                        Name = "Orders",
                        Description = "Place and track your orders",
                        SubFeatures = new List<string>
                        {
                            "Place new orders online",
                            "View all your orders",
                            "Track order status in real-time",
                            "See delivery information",
                            "Receive order notifications"
                        }
                    },
                    new FeatureGuideDto
                    {
                        Name = "Complaints",
                        Description = "Report issues with products or delivery",
                        SubFeatures = new List<string>
                        {
                            "File complaints about products or delivery",
                            "Request returns for defective items",
                            "Upload evidence images",
                            "View complaint status and admin responses",
                            "Track return approvals"
                        }
                    },
                    new FeatureGuideDto
                    {
                        Name = "Notifications",
                        Description = "Stay updated with order information",
                        SubFeatures = new List<string>
                        {
                            "Receive order confirmations",
                            "Get shipping notifications",
                            "Get delivery updates",
                            "Receive complaint responses",
                            "Mark notifications as read"
                        }
                    },
                    new FeatureGuideDto
                    {
                        Name = "Account Management",
                        Description = "Manage your buyer account",
                        SubFeatures = new List<string>
                        {
                            "Update your profile information",
                            "Change your password",
                            "View your account history",
                            "Manage delivery addresses"
                        }
                    }
                }
            };

            return Ok(new ApiResponse<UserGuideDto> { Success = true, Data = guide });
        }
    }
}
