/// <reference types="vite/client" />

declare module 'framer-motion' {
  export const motion: any;
  export const AnimatePresence: any;
}

declare module 'lucide-react' {
  import { FC, SVGAttributes } from 'react';
  type IconProps = SVGAttributes<SVGSVGElement> & { className?: string; size?: number | string };
  type Icon = FC<IconProps>;
  export const Search: Icon;
  export const ShoppingCart: Icon;
  export const Bell: Icon;
  export const User: Icon;
  export const LogOut: Icon;
  export const Menu: Icon;
  export const X: Icon;
  export const Mail: Icon;
  export const Lock: Icon;
  export const Eye: Icon;
  export const EyeOff: Icon;
  export const Phone: Icon;
  export const MapPin: Icon;
  export const CreditCard: Icon;
  export const ArrowLeft: Icon;
  export const Star: Icon;
  export const Truck: Icon;
  export const Shield: Icon;
  export const Clock: Icon;
  export const CheckCircle: Icon;
  export const Package: Icon;
  export const RotateCcw: Icon;
  export const AlertCircle: Icon;
  export const Users: Icon;
  export const TrendingUp: Icon;
  export const BarChart3: Icon;
  export const Zap: Icon;
  export const Facebook: Icon;
  export const Twitter: Icon;
  export const Linkedin: Icon;
  export const ChevronRight: Icon;
  export const Activity: Icon;
  export const Box: Icon;
  export const DollarSign: Icon;
  export const Layers: Icon;
  export const RefreshCw: Icon;
  export const Settings: Icon;
  export const PieChart: Icon;
  export const Calendar: Icon;
  export const Filter: Icon;
  export const Download: Icon;
  export const Plus: Icon;
  export const Minus: Icon;
  export const Trash2: Icon;
  export const Edit: Icon;
  export const MoreVertical: Icon;
  export const ExternalLink: Icon;
  export const ArrowRight: Icon;
  export const ArrowUp: Icon;
  export const ArrowDown: Icon;
  export const Info: Icon;
  export const XCircle: Icon;
  export const File: Icon;
  export const Check: Icon;
  export const Layout: Icon;
  export const LayoutGrid: Icon;
  export const Briefcase: Icon;
  export const ShoppingBag: Icon;
  export const Heart: Icon;
  export const ArrowDownUp: Icon;
}

declare module '@headlessui/react' {
  export const Transition: any;
  export const Dialog: any;
  export const Disclosure: any;
  export const Listbox: any;
  export const Switch: any;
  export const Tab: any;
  export const Popover: any;
}

declare module 'react-icons/*' {
  const icons: { [key: string]: any };
  export = icons;
}

declare module 'axios-mock-adapter' {
  const MockAdapter: any;
  export default MockAdapter;
}
